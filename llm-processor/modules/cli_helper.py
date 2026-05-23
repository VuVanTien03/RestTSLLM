# cli_helper.py

import argparse
from modules.log_helper import LogHelper

class CliHelper:
    def __init__(self):
        """Constructor Method"""
        LogHelper.write('CliHelper', 'Action', f'reading args...')
        self.args = {}
        self.read_args()

    def read_args(self):
        parser = argparse.ArgumentParser()
        
        parser.add_argument('--debug', action=argparse.BooleanOptionalAction, default=False, help='Enable debug mode')
        parser.parse_args(['--no-debug'])
        
        parser.add_argument('--temperature', type=float, default=1.0, help='Set temperature between 0.0 and 1.0 used for all llm (default: 1.0)')
        
        parser.add_argument('--llm-config-file', dest='llm_config_file', type=str, default='config/llms_config.json', help='Set LLM config file with each llm configuration (default: config/llms_config.json)')

        parser.add_argument('--prompts-dir', dest='prompts_dir', type=str, default='prompts', help='Set dir with sequential prompts and system command used for all llm (default: prompts)')
       
        parser.add_argument('--system-command-file', dest='system_command_file', type=str, default='0_system_command.txt', help='Set system command file name inside prompts dir used for all llm (default: 0_system_command.txt)')
        
        parser.add_argument('--output-dir', dest='output_dir', type=str, default='output', help='Set dir to save results used for all llm (default: output)')      
        
        parser.add_argument('--llms', type=str, default='all', help='Set llm list to process separated by comma like \'mixtral,gpt,gemini\' or \'all\' to use all llms from llm-config-file (default: all)') 
        
        self.args = parser.parse_args()
        
        self.args.llms = [x.strip() for x in self.args.llms.split(',')]
        
        print("debug: " + str(self.args.debug))
        print("temperature: " + str(self.args.temperature))
        print("llm-config-file: " + str(self.args.llm_config_file))
        print("prompts-dir: " + str(self.args.prompts_dir))
        print("system-command-file: " + str(self.args.system_command_file))
        print("output-dir: " + str(self.args.output_dir))
        print("llms: " + str(self.args.llms))
        