import { Component, Inject, Injectable } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';

import { IDialogAlert, IDialogConfirm, IDialogPrompt } from '../app.data';

const DIALOG_DEFAULT_WIDTH = '400px';
const DIALOG_DEFAULT_CLOSE_TEXT = 'Close';
const DIALOG_DEFAULT_ACCEPT_TEXT = 'Accept';
const DIALOG_DEFAULT_CONFIRM_TEXT = 'Confirm';
const DIALOG_DEFAULT_CANCEL_TEXT = 'Cancel';

@Injectable()
export class DialogService {
    constructor(private dialog: MatDialog) { }

    public async alert(alert: IDialogAlert): Promise<void> {
        return new Promise<void>((resolve) => {
            const dialogRef = this.dialog.open(DialogAlertComponent, {
                width: alert.width || DIALOG_DEFAULT_WIDTH,
                data: {
                    title: alert.title,
                    message: alert.message,
                    closeButtonText: alert.closeButtonText || DIALOG_DEFAULT_CLOSE_TEXT,
                }
            });
            dialogRef.afterClosed().subscribe(_ => {
                resolve();
            });
        });
    }

    public confirm(confirm: IDialogConfirm): Promise<boolean> {
        return new Promise<boolean>((resolve) => {
            const dialogRef = this.dialog.open(DialogConfirmComponent, {
                width: confirm.width || DIALOG_DEFAULT_WIDTH,
                data: {
                    title: confirm.title,
                    message: confirm.message,
                    confirmButtonText: confirm.confirmButtonText || DIALOG_DEFAULT_CONFIRM_TEXT,
                    cancelButtonText: confirm.cancelButtonText || DIALOG_DEFAULT_CANCEL_TEXT,
                }
            });
            dialogRef.afterClosed().subscribe((result: boolean) => {
                resolve(result);
            });
        });
    }

    public prompt(prompt: IDialogPrompt): Promise<string> {
        return new Promise<string>((resolve) => {
            const dialogRef = this.dialog.open(DialogPromptComponent, {
                width: prompt.width || DIALOG_DEFAULT_WIDTH,
                data: {
                    title: prompt.title,
                    message: prompt.message,
                    value: prompt.value,
                    acceptButtonText: prompt.acceptButtonText || DIALOG_DEFAULT_ACCEPT_TEXT,
                    cancelButtonText: prompt.cancelButtonText || DIALOG_DEFAULT_CANCEL_TEXT,
                }
            });
            dialogRef.afterClosed().subscribe((result: string) => {
                resolve(result);
            });
        });
    }
}

@Component({
    selector: 'app-dialog-alert',
    template: `
    <h1 mat-dialog-title *ngIf="data.title">{{data.title}}</h1>
    <div mat-dialog-content>
    <span class="dialog-message">{{data.message}}</span>
    </div>
    <div mat-dialog-actions class="dialog-actions">
        <button mat-button color="accent" [mat-dialog-close]="true" cdkFocusInitial>{{data.closeButtonText}}</button>
    </div>
    `,
})
export class DialogAlertComponent {
    constructor(
        public dialogRef: MatDialogRef<DialogAlertComponent>,
        @Inject(MAT_DIALOG_DATA) public data: IDialogAlert,
    ) { }

    onNoClick(): void {
        this.dialogRef.close();
    }
}

@Component({
    selector: 'app-dialog-confirm',
    template: `
    <h1 mat-dialog-title *ngIf="data.title">{{data.title}}</h1>
    <div mat-dialog-content>
    <span class="dialog-message">{{data.message}}</span>
    </div>
    <div mat-dialog-actions class="dialog-actions">
        <button mat-button [mat-dialog-close]="false">{{data.cancelButtonText}}</button>
        <button mat-button color="accent" [mat-dialog-close]="true" cdkFocusInitial>{{data.confirmButtonText}}</button>
    </div>
    `,
})
export class DialogConfirmComponent {
    constructor(
        public dialogRef: MatDialogRef<DialogConfirmComponent>,
        @Inject(MAT_DIALOG_DATA) public data: IDialogConfirm,
    ) { }

    onNoClick(): void {
        this.dialogRef.close();
    }
}

@Component({
    selector: 'app-dialog-prompt',
    template: `
    <h1 mat-dialog-title *ngIf="data.title">{{data.title}}</h1>
    <div mat-dialog-content>
    <span class="dialog-message">{{data.message}}</span>
    <mat-form-field class="dialog-input">
        <input matInput [(ngModel)]="data.value" cdkFocusInitial>
    </mat-form-field>
    </div>
    <div mat-dialog-actions class="dialog-actions">
        <button mat-button (click)="onNoClick()">{{data.cancelButtonText}}</button>
        <button mat-button color="accent" [mat-dialog-close]="data.value">{{data.acceptButtonText}}</button>
    </div>
    `,
})
export class DialogPromptComponent {
    constructor(
        public dialogRef: MatDialogRef<DialogPromptComponent>,
        @Inject(MAT_DIALOG_DATA) public data: IDialogPrompt,
    ) { }

    onNoClick(): void {
        this.dialogRef.close();
    }
}