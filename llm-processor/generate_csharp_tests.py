# generate_csharp_tests.py

import os
import re
import subprocess
from pathlib import Path


INPUT_DIR = "../general-files/output/"
OUTPUT_DIR = "../general-files/cshaptest/"


def extract_csharp_files(content: str):
    """
    Extract C# files from markdown/code text.
    Each file must start with:
    // File: FileName.cs
    """

    pattern = re.compile(
        r"```csharp\s*(//\s*File:\s*(?P<filename>[^\n\r]+)\s*(?P<code>.*?))```",
        re.DOTALL | re.IGNORECASE
    )

    files = []

    for match in pattern.finditer(content):
        filename = match.group("filename").strip()
        code = match.group("code").strip()

        full_code = f"// File: {filename}\n\n{code}\n"
        files.append((filename, full_code))

    return files


def write_files(files):
    output_path = Path(OUTPUT_DIR)
    output_path.mkdir(parents=True, exist_ok=True)

    for filename, code in files:
        file_path = output_path / filename

        file_path.write_text(code, encoding="utf-8")
        print(f"Created: {file_path}")


def run_dotnet_test():
    print("\nRunning C# tests with dotnet test...\n")

    result = subprocess.run(
        ["dotnet", "test"],
        cwd=OUTPUT_DIR,
        text=True,
        capture_output=True
    )

    print(result.stdout)

    if result.stderr:
        print(result.stderr)

    if result.returncode != 0:
        print("dotnet test failed.")
    else:
        print("dotnet test completed successfully.")


def main():
    input_dir = Path(INPUT_DIR)
    target_files = [
        "gpt_4_convert_tsl_to_integration_tests_1_result.txt",
        "gpt_5_convert_tsl_to_integration_tests_2_result.txt",
        "gpt_6_convert_tsl_to_integration_tests_3_result.txt"
    ]
    
    all_files = []
    
    for filename in target_files:
        input_path = input_dir / filename
        if not input_path.exists():
            print(f"Input file not found: {input_path}")
            continue

        content = input_path.read_text(encoding="utf-8")
        files = extract_csharp_files(content)
        if files:
            all_files.extend(files)

    if not all_files:
        print("No C# files found.")
        print("Make sure each block starts with: ```csharp and // File: FileName.cs")
        return

    write_files(all_files)

    # Run tests using C#
    run_dotnet_test()


if __name__ == "__main__":
    main()