# configuration.py

import os, json
from dotenv import load_dotenv
from modules.log_helper import LogHelper

class Configuration:
    def __init__(self):
        """Constructor Method"""
        LogHelper.write('Configuration', 'Action', f'loading .env file')
        self.env_file = f'config/.env'
        self.load_env()

    def get_env_var(self, key):
        try:
            return os.getenv(key)
        except:
            return ""
        
    def load_json_with_vars(self, file):
        LogHelper.write('Configuration', 'Action', f'loading json file with vars - file {file}')
        with open(file, 'r', encoding='utf-8') as file_content:
            content = file_content.read()
            
        for key, value in os.environ.items():
            content = content.replace('{' + key + '}', value)
            
        return json.loads(content)
    
    def load_env(self):
        load_dotenv(self.env_file)