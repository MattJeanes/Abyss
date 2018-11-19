import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { JwtHelperService } from "@auth0/angular-jwt";
import { first } from "rxjs/operators";

import { IAuthResult, IAuthScheme, IClientUser, IToken } from "../app.data";

@Injectable()
export class AuthService {
    constructor(private httpClient: HttpClient, private jwtHelperService: JwtHelperService, private router: Router) { }
    public getNewToken(scheme?: string) {
        return this.httpClient.post<IAuthResult>(`/api/auth/token/${scheme ? scheme : ""}`, undefined).pipe(first()).toPromise();
    }
    public getAuthSchemes() {
        return this.httpClient.get<IAuthScheme[]>("/api/auth/schemes").pipe(first()).toPromise();
    }
    public getRawToken() {
        return localStorage.token as string | undefined;
    }
    public getUser() {
        const token = this.getToken();
        return token && this.isLoggedIn() ? JSON.parse(token.User) as IClientUser : undefined;
    }
    public getToken() {
        const token = this.getRawToken();
        if (token) {
            const decodedToken = this.jwtHelperService.decodeToken();
            return decodedToken as IToken;
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
    public tokenNeedsRefresh() {
        const token = this.getToken();
        if (token) {
            //return true;
            return token.exp <= (Date.now().valueOf() / 1000);
        } else {
            throw new Error("Not logged in");
        }
    }
    public isLoggedIn() {
        const token = this.getToken();
        if (token) {
            const refreshExp = parseInt(token.RefreshExpiry);
            return refreshExp > (Date.now().valueOf() / 1000);
        } else {
            return false;
        }
    }
    public async logout(allSessions?: boolean) {
        await this.httpClient.delete<boolean>(`/api/auth/${allSessions !== undefined ? allSessions.toString() : ""}`).pipe(first()).toPromise();
        this.setToken();
        this.router.navigate(["/"]);
    }
    public hasPermission(permissions: string) {
        const user = this.getUser();
        if (user && permissions.split(",").map(x => x.trim()).some(x => user.Permissions.includes(x))) {
            return true;
        }
        return false;
    }
}
