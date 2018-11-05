import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { JwtHelperService } from "@auth0/angular-jwt";
import { first } from "rxjs/operators";

import { IAuthResult, IUser } from "./app.data";

@Injectable()
export class AppService {
    constructor(private httpClient: HttpClient, private jwtHelperService: JwtHelperService) { }
    public login() {
        window.location.replace("/auth/login");
    }
    public getNewToken() {
        return this.httpClient.get<IAuthResult>("/auth/token").pipe(first()).toPromise();
    }
    public getToken() {
        return localStorage.token as string | undefined;
    }
    public getUser() {
        const token = this.getToken();
        if (token && !this.jwtHelperService.isTokenExpired(token)) {
            return this.jwtHelperService.decodeToken() as IUser;
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
}
