import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { HomeComponent } from './home.component';

export const routes: Routes = [
    {
        path: '',
        component: HomeComponent
    },
];

@NgModule({
    imports: [
        RouterModule.forChild(routes),
    ],
    declarations: [
        HomeComponent,
    ],
    providers: [],
})
export class HomeModule { }
