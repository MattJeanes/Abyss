import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { firstValueFrom } from 'rxjs';

import { IAuthResult, IAuthScheme, IClientUser, IToken } from '../app.data';

@Injectable()
export class AuthService {
    constructor(private httpClient: HttpClient, private jwtHelperService: JwtHelperService, private router: Router) { }
    public async getNewToken(scheme?: string): Promise<IAuthResult> {
        const result = await firstValueFrom(this.httpClient.post<IAuthResult>(`/api/auth/token/${scheme ? scheme : ''}`, undefined));
        this.setToken(result.Token);
        return result;
    }
    public getAuthSchemes(): Promise<IAuthScheme[]> {
        return firstValueFrom(this.httpClient.get<IAuthScheme[]>('/api/auth/schemes'));
    }
    public getRawToken(): string | undefined {
        return localStorage.token as string | undefined;
    }
    public getUser(): IClientUser | undefined {
        const token = this.getToken();
        return token && this.isLoggedIn() ? JSON.parse(token.User) as IClientUser : undefined;
    }
    public getToken(): IToken | undefined {
        const token = this.getRawToken();
        if (token) {
            const decodedToken = this.jwtHelperService.decodeToken();
            return decodedToken as IToken;
        }
        return undefined;
    }
    public setToken(token?: string): void {
        if (token) {
            console.log(`Setting token ${token}`);
            localStorage.token = token;
        } else {
            console.log('Deleting token');
            delete localStorage.token;
        }
    }
    public tokenNeedsRefresh(): boolean {
        const token = this.getToken();
        if (token) {
            // return true;
            return token.exp <= (Date.now().valueOf() / 1000);
        } else {
            return false;
        }
    }
    public isLoggedIn(): boolean {
        const token = this.getToken();
        if (token) {
            const refreshExp = parseInt(token.RefreshExpiry);
            return refreshExp > (Date.now().valueOf() / 1000);
        } else {
            return false;
        }
    }
    public async logout(allSessions?: boolean): Promise<void> {
        await firstValueFrom(this.httpClient.delete<boolean>(`/api/auth/${allSessions !== undefined ? allSessions.toString() : ''}`));
        this.setToken();
        this.router.navigate(['/']);
    }
    public hasPermission(permissions: string): boolean {
        const user = this.getUser();
        if (user && permissions.split(',').map(x => x.trim()).some(x => user.Permissions.includes(x))) {
            return true;
        }
        return false;
    }
}
