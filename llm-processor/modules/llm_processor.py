# llm_processor.py

import os, requests, json, copy, shutil, time, decimal
from decimal import Decimal
from jsonpath_ng import parse
from openai import OpenAI
from tqdm import tqdm
from modules.log_helper import LogHelper
from modules.file_helper import FileHelper
from modules.configuration import Configuration

class LLMProcessor:

    def __init__(self, llms_configuration_file, temperature = 1.0, system = None):
        self.configuration = Configuration()
        self.llms_configuration = self.configuration.load_json_with_vars(llms_configuration_file)
        self.all_llms = [x[0] for x in sorted(self.llms_configuration.items(), key=lambda x: str(x[0]))]
        self.system = system
        self.temperature = temperature
        self.default_result_path = "choices[0].message.content"
        self.default_input_tokens_path = "usage.prompt_tokens"
        self.default_output_tokens_path = "usage.completion_tokens"
        self.decimal_context = decimal.Context()
        self.decimal_context.prec = 20
        self.clear_history()
    
    def update_system_command(self, system):
        self.system = system
        self.clear_history()
    
    def clear_history(self):
        self.conversation_history = []
        self.last_input_size = 0
        self.last_output_size = 0
        self.last_total_size = 0
        self.last_input_cost = None
        self.last_output_cost = None
        self.last_total_cost = None
        if (self.system is not None):
            self.conversation_history.append({"role": "system", "content": self.system})
    
    def add_user_history(self, prompt):
        self.conversation_history.append({"role": "user", "content": prompt})
        
    def add_assistant_history(self, result):
        self.conversation_history.append({"role": "assistant", "content": result})
    
    def log_last_conversation(self, llm, file, elapsed):
        
        metrics = self.get_metrics(llm, file, elapsed)
        
        for line in metrics.split('\n'):     
            LogHelper.write('LLMProcessor', 'Debug', f'  metric: {line}') 
            
        for item in self.conversation_history:
            line = str(item) \
                .replace("'user',", "'user',     ") \
                .replace("'system',", "'system',   ")
            LogHelper.write('LLMProcessor', 'Debug', f'  {line}')
            
    def process(self, prompt, llm = 'gpt'):

        llm_config = self.get_llm_configuration(llm)
        self.add_user_history(prompt)

        LogHelper.write('LLMProcessor', 'Debug', f'processing llm: {llm} - model: {llm_config["model"]}')

        headers = llm_config["headers"]
        headers["Content-Type"] = "application/json"

        data = self.get_request_body(llm_config)

        LogHelper.write('LLMProcessor', 'Debug', f'request body:\n: {json.dumps(data)}')

        if self.is_openai_compatible(llm_config):
            client = OpenAI(
                api_key=self.get_api_key(llm_config),
                base_url=self.get_openai_base_url(llm_config["url"])
            )
            response_json = client.chat.completions.create(**data).model_dump()
            LogHelper.write('LLMProcessor', 'Debug', f'response body:\n: {response_json}')
            result = self.get_result_from_json(llm_config, response_json)
        else:
            response = requests.post(llm_config["url"], headers=headers, data=json.dumps(data))
            LogHelper.write('LLMProcessor', 'Debug', f'response body:\n: {response.json()}')
            result = self.get_result(llm_config, response)

        self.add_assistant_history(result)

        LogHelper.write('LLMProcessor', 'Debug', f'result - prompts length: {self.last_input_size} - completions length: {self.last_output_size}')

        return result
    
    def process_many(self, llms, prompts_dir, system_file, output_dir, sleep = 1):
        LogHelper.write('LLMProcessor', 'Action', f'processing prompts in llms') 

        if ("all" in llms):
            llms = self.all_llms

        system = FileHelper.load_file(f'{prompts_dir}/{system_file}')
        self.update_system_command(system)
        
        prompt_files = FileHelper.list_files(prompts_dir)
        prompt_files.remove(system_file)
        
        llms_len = len(llms)
        prompts_len = len(prompt_files)
        total_iteration = llms_len * prompts_len
        
        if os.path.exists(output_dir):
            shutil.rmtree(output_dir)
        
        total_cost = 0
        all_metrics = ""
        
        with tqdm(total = total_iteration) as progress_bar:
            for llm in llms:

                for file in prompt_files:
                    
                    prompt = FileHelper.load_file(f'{prompts_dir}/{file}')
                    
                    start_time = time.time()
                    result = self.process(prompt, llm)
                    elapsed_time_ms = int((time.time() - start_time) * 1000)
                    
                    self.log_last_conversation(llm, file, elapsed_time_ms)
                    
                    file_without_extension = file.split('.')[0]
                    result_file = f'{output_dir}/{llm}_{file_without_extension}_result.txt'
                    metrics = self.get_metrics(llm, file, elapsed_time_ms)
                    FileHelper.write(result_file, result)
                    total_cost += self.last_total_cost
                    all_metrics = f'{all_metrics}{metrics}\n'
                    time.sleep(sleep)
                    
                    llm_config = self.get_llm_configuration(llm)
                    if ("sleep" in llm_config):
                        time.sleep(llm_config["sleep"])
                    
                    progress_bar.update(1)
                    
                self.clear_history()
        
        all_metrics_file = f'{output_dir}/summary_metrics.txt'
        FileHelper.write(all_metrics_file, all_metrics)
        LogHelper.write('LLMProcessor', 'Action', f'finish! - total cost (usd): {total_cost}') 
    
    def get_metrics(self, llm, file, elapsed):
        metrics =  f'{llm}\t{file}\telapsed_time_ms\t%s\n' % elapsed
        metrics += f'{llm}\t{file}\tinput_tokens\t{self.last_input_size}\n'
        metrics += f'{llm}\t{file}\toutput_tokens\t{self.last_output_size}\n'
        metrics += f'{llm}\t{file}\ttotal_tokens\t{self.last_total_size}\n'
        metrics += f'{llm}\t{file}\tinput_cost_usd\t{self.format_decimal(self.last_input_cost)}\n'
        metrics += f'{llm}\t{file}\toutput_cost_usd\t{self.format_decimal(self.last_output_cost)}\n'
        metrics += f'{llm}\t{file}\ttotal_cost_usd\t{self.format_decimal(self.last_total_cost)}'
        
        return metrics
    
    def get_by_json_path(self, data, json_path):
        jsonpath_expr = parse(json_path)
        matches = jsonpath_expr.find(data)
        if matches:
            return matches[0].value
        else:
            return None
    
    def get_result(self, llm_config, response):
        if response.status_code == 200:
            return self.get_result_from_json(llm_config, response.json())
        else:
            raise ValueError(f'wrong status code {response.status_code} {response.json()}')

    def get_result_from_json(self, llm_config, response_json):
        json_path = self.get_dict_value(llm_config, "result_path", self.default_result_path)
        result = self.get_by_json_path(response_json, json_path)

        json_path = self.get_dict_value(llm_config, "input_tokens_path", self.default_input_tokens_path)
        self.last_input_size = self.get_by_json_path(response_json, json_path)

        json_path = self.get_dict_value(llm_config, "output_tokens_path", self.default_output_tokens_path)
        self.last_output_size = self.get_by_json_path(response_json, json_path)

        self.last_total_size = self.last_input_size + self.last_output_size

        if ("cost_by_million" in llm_config):
            self.last_input_cost = (Decimal(self.last_input_size) / Decimal(1000000)) * Decimal(llm_config["cost_by_million"]["input"])
            self.last_output_cost = (Decimal(self.last_output_size) / Decimal(1000000)) * Decimal(llm_config["cost_by_million"]["output"])
            self.last_total_cost = self.last_input_cost + self.last_output_cost

        return result

    def is_openai_compatible(self, llm_config):
        return llm_config["url"].endswith("/chat/completions")

    def get_openai_base_url(self, url):
        return url.rsplit("/chat/completions", 1)[0]

    def get_api_key(self, llm_config):
        authorization = llm_config.get("headers", {}).get("Authorization", "")
        if authorization.startswith("Bearer "):
            return authorization.split("Bearer ", 1)[1]

        api_key = llm_config.get("headers", {}).get("x-api-key")
        if api_key:
            return api_key

        raise ValueError("API key not found in llm headers")
    
    def format_decimal(self, value):
        return f"{value:.8f}".replace('.', ',')
    
    def get_request_body(self, llm_config):
        
        data = {
            "model": llm_config["model"], 
            "messages": copy.deepcopy(self.conversation_history),
            "temperature": self.temperature
        }
        
        if ("max_tokens" in llm_config):
            data["max_tokens"] = llm_config["max_tokens"]
        
        if ("claude" in llm_config["model"]):
            data["system"] = self.system
            data["messages"] = [message for message in data["messages"] if message["role"] != "system"]
            
        if ("gemini" in llm_config["model"]):
            data["contents"] = data["messages"]
            del data["messages"]
            del data["model"]

            data["generationConfig"] = {
                "temperature": data['temperature']
            }
            del data['temperature']
            
            if ("max_tokens" in llm_config):
                data["generationConfig"]["maxOutputTokens"] = llm_config["max_tokens"]
                del data["max_tokens"]

            for message in data["contents"]:
                message["parts"] = []
                message["parts"].append({"text": message["content"]})
                del message["content"]
                
                if message["role"] == "assistant":
                    message["role"] = "model"
                    
                if message["role"] == "system":
                    data["system_instruction"] = { "parts": message["parts"]}
            data["contents"] = [message for message in data["contents"] if message["role"] != "system"]
        
        return data
    
    def get_llm_configuration(self, llm):
        if llm not in self.llms_configuration:
            raise ValueError('invalid llm')
            
        return self.llms_configuration[llm]
    
    def get_dict_value(self, dict, key, default):
        value = default
        
        if (key in dict):
            value = dict[key]
            
        return value