import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { first } from 'rxjs/operators';

import { AuthService, DialogService } from './services';

@Component({
    template: 'Please wait, logging in..',
})
export class LoginComponent implements OnInit {
    constructor(private authService: AuthService, private router: Router, private activatedRoute: ActivatedRoute, private dialogService: DialogService) { }

    public async ngOnInit(): Promise<void> {
        try {
            const params = await this.activatedRoute.params.pipe(first()).toPromise();
            await this.authService.getNewToken(params.scheme);
            this.router.navigate(['/']);
        } catch (e: any) {
            this.dialogService.openAlert({
                title: 'Failed to login',
                message: e.toString(),
            });
        }
    }
}
