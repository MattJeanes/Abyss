import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { IUser } from '../app.data';
import { UserManagerService } from './usermanager.service';

import { DialogService } from '../services';

@Component({
    templateUrl: './usermanager.component.html',
    imports: [
        CommonModule
    ],
    providers: [
        UserManagerService,
    ]
})
export class UserManagerComponent implements OnInit {
    public users: IUser[] = [];

    constructor(public userManagerService: UserManagerService, public dialogService: DialogService) { }

    public async ngOnInit(): Promise<void> {
        try {
            this.users = await this.userManagerService.getUsers();
        } catch (e: any) {
            this.dialogService.alert({
                title: 'Failed to load user manager',
                message: e.toString(),
            });
        }
    }
}
