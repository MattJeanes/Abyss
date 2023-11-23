import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { MatLegacySelectModule as MatSelectModule } from '@angular/material/legacy-select'
import { MatLegacyButtonModule as MatButtonModule } from '@angular/material/legacy-button';
import { MatLegacyProgressSpinnerModule as MatProgressSpinnerModule } from '@angular/material/legacy-progress-spinner';

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
