import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { MatButtonModule } from '@angular/material/button';

import { MomentModule } from "ngx-moment";

import { OnlineComponent } from './online.component';
import { OnlineService } from './online.service';

export const routes: Routes = [
    {
        path: '',
        component: OnlineComponent
    },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
        CommonModule,
        MomentModule,
        MatButtonModule,
    ],
    declarations: [
        OnlineComponent,
    ],
    providers: [
        OnlineService,
    ],
})
export class OnlineModule { }
