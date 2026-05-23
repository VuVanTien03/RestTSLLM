# file_helper.py

import os, shutil
from pathlib import Path

class FileHelper:
    def write(path, content): 
        output_file = Path(path)
        output_file.parent.mkdir(exist_ok=True, parents=True)
        output_file.write_text(content, encoding='utf-8')
        
    def load_file(file):
        with open(file, 'r', encoding='utf-8') as file_content:
            content = file_content.read()
            
        return content
    
    def list_files(dir):
        return os.listdir(dir)

    def copy_dir(src, dest): 
        shutil.copytree(src, dest, dirs_exist_ok=True)
