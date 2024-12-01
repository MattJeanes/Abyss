import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ThemePalette } from '@angular/material/core';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { filter, map } from 'rxjs/operators';

import { TitleService, AuthService, DialogService } from './services';
import { Permissions } from './app.data';
import { ErrorService } from './services/error.service';
import { AccountDialogComponent } from './shared/account-dialog.component';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
    standalone: false
})
export class AppComponent implements OnInit {
    public Permissions = Permissions;
    constructor(
        public authService: AuthService,
        public errorService: ErrorService,
        public router: Router,
        private dialog: MatDialog,
        private dialogService: DialogService,
        private titleService: TitleService,
        private activatedRoute: ActivatedRoute,
    ) {
        this.router.events
            .pipe(
                filter(event => event instanceof NavigationEnd),
                map(() => this.activatedRoute),
                map((route: ActivatedRoute) => {
                    while (route.firstChild) route = route.firstChild;
                    return route;
                }),
                filter(route => route.outlet === 'primary'),
            )
            .subscribe((route: ActivatedRoute) => {
                this.setPageTitle(route);
            });
    }

    public async ngOnInit(): Promise<void> {
        try {
            if (this.authService.isLoggedIn()) {
                await this.authService.getNewToken();
            }
        } catch (e: any) {
            this.dialogService.alert({
                title: 'Failed to get new auth token, forcing logout',
                message: e.toString(),
            });
            try {
                await this.authService.logout();
            } catch (e: any) {
                this.dialogService.alert({
                    title: 'Failed to log out',
                    message: e.toString(),
                });
            }
        }
    }

    public get username(): string {
        const user = this.authService.getUser();
        if (user) {
            return user.Name;
        } else {
            return 'Unknown';
        }
    }

    public showAccountDialog(): void {
        this.dialog.open(AccountDialogComponent);
    }

    public getTabColor(url: string): ThemePalette {
        return this.router.url == url ? 'accent' : undefined;
    }

    private setPageTitle(route: ActivatedRoute): void {
        const snapshot = route.snapshot;
        const data = snapshot.data;
        const pageTitle = data.pageTitle;
        if (pageTitle) {
            this.titleService.setTitle(pageTitle);
        }
    }
}
