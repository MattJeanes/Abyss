import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

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
        const result = await firstValueFrom(this.httpClient.post<IAuthResult>('/api/user/username', username));
        this.authService.setToken(result.Token);
    }
    public getUsers(): Promise<IUser[]> {
        return firstValueFrom(this.httpClient.get<IUser[]>('/api/user'));
    }
    public async deleteAuthScheme(schemeId: string): Promise<void> {
        const result = await firstValueFrom(this.httpClient.delete<IAuthResult>(`/api/user/scheme/${schemeId}`));
        this.authService.setToken(result.Token);
    }
}
