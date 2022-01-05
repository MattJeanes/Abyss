import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { IWhoSaid } from '../app.data';

@Injectable()
export class WhoSaidService {
    constructor(private httpClient: HttpClient) { }
    public whoSaid(message: string): Promise<IWhoSaid> {
        return firstValueFrom(this.httpClient.post<IWhoSaid>('/api/whosaid', message));
    }
}
