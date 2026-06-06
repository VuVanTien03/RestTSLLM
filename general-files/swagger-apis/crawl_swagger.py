import requests
import json

BASE_URL = "http://localhost:8005"
OPENAPI_URL = f"{BASE_URL}/openapi.json"

def download_swagger():
    try:
        response = requests.get(OPENAPI_URL, timeout=30)
        response.raise_for_status()

        swagger_data = response.json()

        with open("swagger.json", "w", encoding="utf-8") as f:
            json.dump(swagger_data, f, ensure_ascii=False, indent=2)

        print("Tải Swagger/OpenAPI thành công!")
        print("File đã lưu: swagger.json")

    except requests.exceptions.ConnectionError:
        print("Không kết nối được tới server. Kiểm tra lại IP, port hoặc server đã chạy chưa.")

    except requests.exceptions.Timeout:
        print("Request bị timeout.")

    except requests.exceptions.HTTPError as e:
        print(f"Lỗi HTTP: {e}")

    except Exception as e:
        print(f"Lỗi khác: {e}")


if __name__ == "__main__":
    download_swagger()