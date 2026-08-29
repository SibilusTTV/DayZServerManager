import {Component, inject, signal, WritableSignal} from '@angular/core';
import {InstanceService} from '../../api';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle
} from '@angular/material/dialog';
import {MatFormField, MatInput, MatLabel} from '@angular/material/input';
import {FormsModule} from '@angular/forms';
import {MatButton} from '@angular/material/button';

export interface BanWindowProps {
  instanceId: string;
  guid: string;
}

@Component({
  selector: 'app-ban-window',
  imports: [
    MatFormField,
    MatLabel,
    MatInput,
    FormsModule,
    MatDialogContent,
    MatDialogActions,
    MatButton,
    MatDialogTitle
  ],
  templateUrl: './ban-window.html'
})
export class BanWindow {
  private readonly dialogRef = inject(MatDialogRef<BanWindow>);
  private readonly data = inject<BanWindowProps>(MAT_DIALOG_DATA);
  public guid: string = this.data.guid;
  public instanceId: string = this.data.instanceId;

  public banReason: WritableSignal<string> = signal("");
  public banDuration: WritableSignal<number> = signal(0);

  public onBanClick(): void {
    InstanceService.getApiInstanceBanPlayer(this.guid, this.instanceId, this.banReason(), this.banDuration()).finally(() => {
      this.dialogRef.close();
    });
  }

  onNoClick(): void {
    this.dialogRef.close();
  }
}
