import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { MatSelectModule } from '@angular/material/select'
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ServerManagerComponent } from './servermanager.component';
import { ServerManagerService } from './servermanager.service';

import { AuthGuard } from '../services/auth.guard';

export const routes: Routes = [
    {
        path: '',
        component: ServerManagerComponent,
        canActivate: [AuthGuard],
        data: { permissions: 'ServerManager' },
    },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        FormsModule,
        CommonModule,
        MatSelectModule,
        MatButtonModule,
        MatProgressSpinnerModule,
    ],
    declarations: [
        ServerManagerComponent,
    ],
    providers: [
        ServerManagerService
    ],
})
export class ServerManagerModule { }
