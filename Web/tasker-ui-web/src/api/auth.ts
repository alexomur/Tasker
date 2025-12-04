const AUTH_API_BASE_URL =
  import.meta.env.VITE_AUTH_API_BASE_URL ?? "http://localhost:5001/api/v1";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  userId: string;
  accessToken: string;
  expiresInSeconds: number;
}

export async function login(payload: LoginRequest): Promise<LoginResponse> {
  const response = await fetch(`${AUTH_API_BASE_URL}/auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    let details = "";

    try {
      details = await response.text();
    } catch {
      details = "";
    }

    throw new Error(
      `HTTP ${response.status} ${response.statusText}${
        details ? `: ${details}` : ""
      }`
    );
  }

  const data = (await response.json()) as LoginResponse;
  return data;
}
