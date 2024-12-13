import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';

import { AuthSchemeType, IAuthentication, IClientUser, IUser } from '../app.data';
import { UserManagerService } from './usermanager.service';
import { DialogService } from '../services';

@Component({
    templateUrl: './usermanager.component.html',
    imports: [
        CommonModule,
        MatTableModule,
        MatCardModule,
        MatProgressSpinnerModule,
        MatButtonModule,
    ],
    providers: [
        UserManagerService,
    ]
})
export class UserManagerComponent implements OnInit {
    public users: IUser[] = [];
    public displayedColumns = ['id', 'name', 'roleName', 'authTypes'];
    public loading: boolean = false;

    constructor(public userManagerService: UserManagerService, public dialogService: DialogService) { }

    public async ngOnInit(): Promise<void> {
        await this.refresh();
    }

    public async refresh(): Promise<void> {
        try {
            this.users = [];
            this.loading = true;
            this.users = await this.userManagerService.getUsers();
        } catch (e: any) {
            this.dialogService.alert({
                title: 'Failed to refresh user manager',
                message: e.toString(),
            });
        } finally {
            this.loading = false;
        }
    }

    public getAuthTypes(authentications: IAuthentication[]): string {
        return authentications.map(x => AuthSchemeType[x.SchemeType]).join(', ');
    }
}
