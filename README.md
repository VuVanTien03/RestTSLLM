# 🧩 Integration Test Generation Using LLMs and TSL

This repository contains the artifacts for the study **"Combining TSL and LLMs to Automate REST API Testing: A Comparative Study"**, which evaluates the use of large language models (LLMs) to generate integration tests for REST APIs based on OpenAPI specifications with an intermediate step using TSL.

## 📂 Repository Structure

```
├── general-files/
│   ├── default-prompts/           # Prompt examples used in the few-shot and decomposed prompting strategy
│   ├── default-english-prompts/   # Prompt examples in english version
│   ├── examples/                  # OpenAPI, TSL and Integration Tests examples used to teach LLMs 
├── llm-processor/                 # Script for LLM interaction and execution
├── projects/
│   └── <project-name>/            # Cloned API projects with OpenAPI and results
│       └── prompt-engineering/
│           ├── prompts/           # Input - Specific prompt used in this project
│           ├── output/            # Output - Generated test cases and integration test code for each LLM in this project
├── pdfs/                          # Public evidence files (e.g., results summary)
```

## 🚀 How to use LLM Processor

Check instructions [here](llm-processor/README.md).

## 🧪 Requirements 

- Simple computer with a minimum of 4 GB RAM;
- Python 3;
- Python PIP 3;
- Python Virtual Environment (venv);
- Some LLM API Key with credits or just authorization to use;

Example environment configuration for running on Ubuntu 24.04 operating system:

```
sudo -i
apt update
apt install git python3 python3-venv -y
mkdir /git 
cd /git
git clone https://github.com/ThiagoBarradas/integration-test-with-llm.git
cd llm-processor
python3 -m venv .venv
source .venv/bin/activate
```

After these commands, your environment will be ready to run. Follow the instructions in the next section (How to reproduce the experiment) and then close the Python virtual environment with the `deactivate` command in your terminal.

## 🧪 How to reproduce the experiment

1. Into `llm-processor/config` dir, make a copy from `.env.sample` to `.env` and add your API keys to each LLM provider. You can add only a few API keys if you only want to test a few LLMs.
2. From root, run the follow command to process LLMs for each project that you want to reproduce. If you only want to reproduce the results for just one or few projects, feel free to run the command for whatever project you want (change `<project>` for the available projects `todo-api`, `hotels-api`, `restaurants-api`, `books-api`, `supermarket-api`, and/or `shortener-api`). You can run all LLMs if you have added all API keys or, just change the parameter `--llms=<model>` to point to a specific model (`claude`, `qwen`, `mistral`, `gpt`, `sabia`, `deepseek`, `llama`, or `gemini`)
```
cd llm-processor
pip3 install -r ./requirements.txt
python3 main.py --prompts-dir="../projects/<project>/prompt-engineering/prompts" \
                --system-command-file="0_system_command.txt" \
                --output-dir="../projects/<project>/prompt-engineering/output" \
                --llms=<model>
```
3. After it, you can open a project into Visual Studio (from `projects/<project>/<project>-llm-<model>`) and copy results (from `projects/<project>/prompt-engineering/output`) and paste into existing test project.
4. You can run tests from IDE to verify the results from coverage and to collect mutation score you only need to run the follow command `dotnet stryker --output llm-test-generation-files`, but, some projects have some dependencies and you need to provide executing docker for example. The dependecies and full CLI commands for each project you can find in `dependencies-and-commands.yml`.

## 📊 Results

The study evaluated multiple LLMs (GPT 4o (OpenAI), LLaMA 3.2 90b (Meta), Claude 3.5 Sonnet (Anthropic), Gemini 1.5 Pro (Google), Deepseek R1 (Deepseek), Mistral Large (Mistral), Qwen 2.5 32b (Alibaba), and Sabiá 3 (Maritaca)) using metrics such as:
- ✅ Success Rate
- 📈 Code Coverage
- 🧬 Mutation Score
- 💰 Cost per Execution

Result can be found in:
- Output: [projects](projects/files.md).
- Metrics: [pdfs](pdfs/all_results.pdf).

## 📄 Paper

- [Official Paper - Combining TSL and LLM to Automate REST API Testing A Comparative Study](https://tinyurl.com/resttsllm-paper) 
- [PDF Copy Paper - Combining TSL and LLM to Automate REST API Testing A Comparative Study](pdfs/paper.pdf) 

## 📚 Citation
[![Cite this paper](https://img.shields.io/badge/Cite%20this%20paper-SBES%202025-blue)](#citation)

If you use **RestTSLLM** in your research or project, please cite:

```bibtex
@inproceedings{barradas2025resttsllm,
 author = {{Barradas}, Thiago and {Paes}, Aline and {Neves}, Vania},
 title = {Combining TSL and LLM to Automate REST API Testing: A Comparative Study},
 booktitle = {Proceedings of the 39th Brazilian Symposium on Software Engineering (SBES)},
 location = {Recife/PE},
 year = {2025},
 keywords = {Test Automation, Large Language Models, Integration Testing, REST API Testing, AI in Software Testing, Test Generation},
 issn = {2833-0633},
 pages = {71--81},
 publisher = {SBC},
 address = {Porto Alegre, RS, Brasil},
 doi = {10.5753/sbes.2025.9670},
 url = {https://sol.sbc.org.br/index.php/sbes/article/view/36987}
}
```

**Text citation format:**

```
Barradas, T., Paes, A., & Neves, V. O. (2025). Combining TSL and LLM to Automate REST API Testing: A Comparative Study. In Proceedings of
the 39th Brazilian Symposium on Software Engineering (SBES) (Recife/PE). SBC, Porto Alegre, RS, Brasil, 71–81. doi:10.5753/sbes.2025.9670
```

