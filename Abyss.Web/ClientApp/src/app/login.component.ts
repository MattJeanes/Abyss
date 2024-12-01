import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AuthService, DialogService } from './services';

@Component({
    template: 'Please wait, logging in..',
    standalone: false
})
export class LoginComponent implements OnInit {
    constructor(private authService: AuthService, private router: Router, private activatedRoute: ActivatedRoute, private dialogService: DialogService) { }

    public async ngOnInit(): Promise<void> {
        try {
            const params = await firstValueFrom(this.activatedRoute.params);
            await this.authService.getNewToken(params.scheme);
            this.router.navigate(['/']);
        } catch (e: any) {
            this.dialogService.alert({
                title: 'Failed to login',
                message: e.toString(),
            });
        }
    }
}
