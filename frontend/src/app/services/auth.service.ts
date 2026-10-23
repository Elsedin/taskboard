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