## Running

1. Add more LLM configurations if you need to integrate with more services. With default we have `claude`, `qwen`, `mistral`, `gpt`, `sabia`, `deepseek`, `llama`, and `gemini`. You can add any LLM compatible with [OpenAI API Standard](https://platform.openai.com/docs/api-reference/introduction) (most services are compatible). We have an ACL to integrate with Gemini and Claude, because their API design is the only one different from the standard proposed by the pioneer OpenAI.
2. Copy `config/.env.sample` to `config/.env` and add your API Keys for each LLM API.
3. Create a directory with your prompts (e.g. `prompts/`) sequentially in alphabetical order. We suggest using an ordered prefix like `001_foo.txt`, `002_bar.txt`, etc.
4. Create a directory to receive all results (e.g. `output/`). Note that each run clears the directory before producing new ones. Be sure to copy them if necessary before a new run.
4. Run `python3 -m pip install -r .\requirements.txt` to install dependencies.
5. Run `python3 main.py --prompts-dir="./prompts" --output-dir="./output" --system-command-file=0_system.txt --llms=claude,gpt,mistral,...`. 
    - `--prompts-dir` - Set prompts (inputs) directory. Prompts will be executed in a sequentially alphabetical order.
    - `--output-dir` - Set output directory. Always will be cleaned before a new execution.
    - `--system-command-file` - Set the system (behavior) command. You must set this arg with a filename from prompts dir. This file will be ignored from prompts execution and will be used only to set the system command. e.g. `--system-command-file=000_system.txt`.
    - `--llms` - Choose models to run. You can use `--llms=all` to run for all available models in `config/llm_configs.json` or the llm config name separeted by comma. 
    - `--temperature` - Set the temperature between 0..1 using arg. e.g. `--temperature=0.7`.
    - `--debug` - Enables debug mode.
