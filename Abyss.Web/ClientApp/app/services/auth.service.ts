import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { JwtHelperService } from "@auth0/angular-jwt";
import { first } from "rxjs/operators";

import { IAuthResult, IAuthScheme, IUser } from "../app.data";

@Injectable()
export class AuthService {
    constructor(private httpClient: HttpClient, private jwtHelperService: JwtHelperService) { }
    public getNewToken(scheme?: string) {
        return this.httpClient.get<IAuthResult>(`/api/auth/token/${scheme}`).pipe(first()).toPromise();
    }
    public getAuthSchemes() {
        return this.httpClient.get<IAuthScheme[]>("/api/auth/schemes").pipe(first()).toPromise();
    }
    public getToken() {
        return localStorage.token as string | undefined;
    }
    public getUser() {
        const token = this.getToken();
        if (token && !this.jwtHelperService.isTokenExpired(token)) {
            const decodedToken = this.jwtHelperService.decodeToken();
            return JSON.parse(decodedToken.user) as IUser;
        }
        return undefined;
    }
    public setToken(token?: string) {
        if (token) {
            localStorage.token = token;
        } else {
            delete localStorage.token;
        }
    }
    public isLoggedIn() {
        const token = this.getToken();
        return token && !this.jwtHelperService.isTokenExpired(token);
    }
    public logout() {
        this.setToken();
    }
    public async deleteAuthScheme(schemeId: string) {
        const result = await this.httpClient.delete<IAuthResult>(`/api/auth/${schemeId}`).pipe(first()).toPromise();
        this.setToken(result.Token);
    }
}
