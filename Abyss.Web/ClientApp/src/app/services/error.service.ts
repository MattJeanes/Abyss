import { Injectable } from '@angular/core';

import { AuthService } from './auth.service';

@Injectable()
export class ErrorService {
    constructor(private authService: AuthService) { }
    public async openErrors(): Promise<void> {
        const res = await this.authService.getNewToken();
        window.open(`/errors?token=${res.Token}`, '_blank');
    }
}
