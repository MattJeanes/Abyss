import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { first } from 'rxjs/operators';

import { IWhoSaid } from '../app.data';

@Injectable()
export class WhoSaidService {
    constructor(private httpClient: HttpClient) { }
    public whoSaid(message: string): Promise<IWhoSaid> {
        return this.httpClient.post<IWhoSaid>('/api/whosaid', message).pipe(first()).toPromise();
    }
}
