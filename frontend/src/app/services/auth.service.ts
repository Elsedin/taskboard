import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { Observable, tap } from "rxjs";
import { environment } from "../../environments/environment";

export interface LoginRequest {
    email: string;
    password: string;
}

export interface LoginResponse {
    token:string;
    email: string;
    userId: number;
}

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface RegisterResponse {
  id: number;
  email: string;
}

@Injectable({
    providedIn: 'root',
})
export class AuthService {
    private readonly http = inject(HttpClient);
    private readonly tokenKey = 'taskboard_token';

    login(payload: LoginRequest): Observable<LoginResponse> {
        return this.http
            .post<LoginResponse>(`${environment.apiUrl}/api/auth/login`, payload)
            .pipe(
                tap((res) => {
                    localStorage.setItem(this.tokenKey, res.token);
                }),
            );
    }

    register(payload: RegisterRequest): Observable<RegisterResponse> {
        return this.http.post<RegisterResponse>(
            `${environment.apiUrl}/api/auth/register`,
            payload,
        );
    }

    getToken(): string | null {
        return localStorage.getItem(this.tokenKey);
    }

    logout(): void {
        localStorage.removeItem(this.tokenKey);
    }

    isLoggedIn(): boolean {
        return !!this.getToken();
    }
}