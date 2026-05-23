# main.py

from modules.llm_processor import LLMProcessor
from modules.log_helper import LogHelper
from modules.cli_helper import CliHelper

## receiving args
cli = CliHelper()

## Setting vars
LogHelper.debug = cli.args.debug
processor = LLMProcessor(cli.args.llm_config_file,  cli.args.temperature)
processor.process_many(cli.args.llms, cli.args.prompts_dir, cli.args.system_command_file, cli.args.output_dir)

# command
# python3 main.py --prompts-dir="../projects/supermarket-api/prompt-engineering/prompts" --output-dir="../projects/supermarket-api/prompt-engineering/output"