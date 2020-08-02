import { Component, OnInit } from '@angular/core';
import { TdDialogService } from '@covalent/core/dialogs';

import { IUser } from '../app.data';
import { UserManagerService } from './usermanager.service';

@Component({
    templateUrl: './usermanager.component.html',
})
export class UserManagerComponent implements OnInit {
    public users: IUser[] = [];

    constructor(public userManagerService: UserManagerService, public dialogService: TdDialogService) { }

    public async ngOnInit(): Promise<void> {
        try {
            this.users = await this.userManagerService.getUsers();
        } catch (e) {
            this.dialogService.openAlert({
                title: 'Failed to load user manager',
                message: e.toString(),
            });
        }
    }
}
