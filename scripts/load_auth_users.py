import concurrent.futures
import json
import random
import string
import time
import uuid
import urllib.error
import urllib.request

BASE_URL = "http://127.0.0.1:8080/api/auth"
TOTAL_USERS = 1000
FAIL_RATE = 0.2  # доля попыток логина с неправильным паролем
WORKERS = 30


def _json_post(url: str, payload: dict, timeout: float = 10.0):
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(
        url,
        data=data,
        headers={"Content-Type": "application/json"},
        method="POST",
    )
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        return resp.status, resp.read()


def register_and_login(i: int):
    email = f"user{i}-{uuid.uuid4().hex[:8]}@example.com"
    password_ok = "P@ssw0rd!" + "".join(random.choices(string.ascii_letters + string.digits, k=6))
    display = f"User{i}"

    result = {"register": None, "login_ok": None, "login_fail": None}

    # Регистрация
    try:
        status, _ = _json_post(
            f"{BASE_URL}/register",
            {"email": email, "displayName": display, "password": password_ok},
        )
        result["register"] = status
    except urllib.error.HTTPError as e:
        result["register"] = f"http_error_{e.code}"
        return result
    except Exception as e:
        result["register"] = f"error_{type(e).__name__}"
        return result

    # Успешный логин
    try:
        status, _ = _json_post(
            f"{BASE_URL}/login",
            {"email": email, "password": password_ok},
        )
        result["login_ok"] = status
    except urllib.error.HTTPError as e:
        result["login_ok"] = f"http_error_{e.code}"
    except Exception as e:
        result["login_ok"] = f"error_{type(e).__name__}"

    # Неуспешный логин (для части пользователей)
    if random.random() < FAIL_RATE:
        try:
            status, _ = _json_post(
                f"{BASE_URL}/login",
                {"email": email, "password": password_ok + "wrong"},
            )
            result["login_fail"] = status  # если вдруг 200
        except urllib.error.HTTPError as e:
            result["login_fail"] = f"http_error_{e.code}"
        except Exception as e:
            result["login_fail"] = f"error_{type(e).__name__}"

    return result


def main():
    started = time.time()
    results = []
    with concurrent.futures.ThreadPoolExecutor(max_workers=WORKERS) as pool:
        for res in pool.map(register_and_login, range(TOTAL_USERS)):
            results.append(res)
    took = time.time() - started

    def count(key, prefix="http_error_"):
        return sum(1 for r in results if str(r.get(key, "" ) or "").startswith(prefix))

    register_ok = sum(1 for r in results if r.get("register") == 201)
    register_fail = len(results) - register_ok
    login_ok = sum(1 for r in results if r.get("login_ok") == 200)
    login_fail_http = count("login_fail")
    login_fail_other = sum(
        1
        for r in results
        if r.get("login_fail") not in (None, "") and not str(r.get("login_fail")).startswith("http_error_")
    )

    print(f"Done in {took:.2f}s")
    print(f"Registered ok: {register_ok}, failed: {register_fail}")
    print(f"Login success: {login_ok}")
    print(f"Login failed attempts (expected 401): {login_fail_http}, other errors: {login_fail_other}")


if __name__ == "__main__":
    main()
