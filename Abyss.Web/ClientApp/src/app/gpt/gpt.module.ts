import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';

import { AuthGuard } from '../services/auth.guard';

import { GPTComponent } from './gpt.component';
import { GPTService } from './gpt.service';

export const routes: Routes = [
    {
        path: '',
        component: GPTComponent,
        canActivate: [AuthGuard],
        data: { permissions: 'WhoSaid' },
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
        MatSelectModule,
    ],
    declarations: [
        GPTComponent,
    ],
    providers: [
        GPTService,
    ],
})
export class GPTModule { }
