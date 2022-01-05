import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { IUser } from '../app.data';

@Injectable()
export class UserManagerService {
    constructor(private httpClient: HttpClient) { }
    public getUsers(): Promise<IUser[]> {
        return firstValueFrom(this.httpClient.get<IUser[]>('/api/user'));
    }
}
