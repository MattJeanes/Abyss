import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { MatDialog } from "@angular/material";
import { JwtHelperService } from "@auth0/angular-jwt";
import { TdDialogService } from "@covalent/core";
import { first } from "rxjs/operators";

import { IAuthResult, IUser } from "./app.data";
import { LoginDialogComponent } from "./shared/login-dialog.component";

@Injectable()
export class AppService {
    constructor(private httpClient: HttpClient, private jwtHelperService: JwtHelperService, public dialog: MatDialog, public dialogService: TdDialogService) { }
    public async login() {
        const dialogRef = this.dialog.open(LoginDialogComponent);
        const scheme: string = await dialogRef.afterClosed().toPromise();
        if (scheme) {
            window.location.replace(`/auth/login/${scheme}`);
        }
    }
    public getNewToken(scheme?: string) {
        return this.httpClient.get<IAuthResult>(`/auth/token/${scheme}`).pipe(first()).toPromise();
    }
    public async changeUsername(username?: string) {
        if (!username) {
            const user = this.getUser();
            username = await this.dialogService.openPrompt({
                message: "Enter new username",
                title: "Change username",
                value: user ? user.Name : undefined,
            }).afterClosed().toPromise();
            console.log(`Changing username to ${username}`);
        }
        const newToken = await this.httpClient.post<IAuthResult>(`/auth/username`, username).pipe(first()).toPromise();
        this.setToken(newToken.Token);
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
}
