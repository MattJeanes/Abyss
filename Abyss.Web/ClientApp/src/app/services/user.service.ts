import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { first } from 'rxjs/operators';

import { IAuthResult, IUser } from '../app.data';
import { AuthService } from './auth.service';

import { DialogService } from './dialog.service';
@Injectable()
export class UserService {
    constructor(private httpClient: HttpClient, private authService: AuthService, private dialogService: DialogService) { }
    public async changeUsername(username?: string): Promise<void> {
        const user = this.authService.getUser();
        const currentUsername = user ? user.Name : undefined;
        if (!username) {
            username = await this.dialogService.prompt({
                message: 'Enter new username',
                title: 'Change username',
                value: currentUsername,
            });
            if (username) {
                username = username.trim();
            }
        }
        if (!username || username === currentUsername) { return; }
        console.log(`Changing username to ${username}`);
        const result = await this.httpClient.post<IAuthResult>('/api/user/username', username).pipe(first()).toPromise();
        this.authService.setToken(result.Token);
    }
    public getUsers(): Promise<IUser[]> {
        return this.httpClient.get<IUser[]>('/api/user').pipe(first()).toPromise();
    }
    public async deleteAuthScheme(schemeId: string): Promise<void> {
        const result = await this.httpClient.delete<IAuthResult>(`/api/user/scheme/${schemeId}`).pipe(first()).toPromise();
        this.authService.setToken(result.Token);
    }
}
