import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { MatIconModule } from "@angular/material/icon";
import { MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';

import { AuthGuard } from '../services/auth.guard';

import { WhoSaidComponent } from './whosaid.component';
import { WhoSaidService } from './whosaid.service';

export const routes: Routes = [
    {
        path: '',
        component: WhoSaidComponent,
        canActivate: [AuthGuard],
        data: { permissions: "WhoSaid" },
    },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        CommonModule,
        FormsModule,
        MatIconModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
    ],
    declarations: [
        WhoSaidComponent,
    ],
    providers: [
        WhoSaidService,
    ],
})
export class WhoSaidModule { }
