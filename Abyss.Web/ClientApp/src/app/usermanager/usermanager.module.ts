import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { UserManagerComponent } from './usermanager.component';
import { UserManagerService } from './usermanager.service';
import { AuthGuard } from '../services/auth.guard';

export const routes: Routes = [
    {
        path: '',
        component: UserManagerComponent,
        canActivate: [AuthGuard],
        data: { permissions: 'UserManager' },
    },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        CommonModule,
    ],
    declarations: [
        UserManagerComponent,
    ],
    providers: [
        UserManagerService
    ],
})
export class UserManagerModule { }
