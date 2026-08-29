import {Component, inject, signal, WritableSignal} from '@angular/core';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle
} from '@angular/material/dialog';
import {InstanceService} from '../../api';
import {FormsModule} from '@angular/forms';
import {MatButton} from '@angular/material/button';
import {MatFormField, MatInput, MatLabel} from '@angular/material/input';

export interface KickWindowProps {
  instanceId: string;
  guid: string;
}

@Component({
  selector: 'kick-window',
  templateUrl: 'kick-window.html',
  imports: [
    FormsModule,
    MatButton,
    MatDialogActions,
    MatDialogContent,
    MatDialogTitle,
    MatFormField,
    MatInput,
    MatLabel
  ]
})
export class KickWindow {
  private readonly dialogRef = inject(MatDialogRef<KickWindow>);
  private readonly data = inject<KickWindowProps>(MAT_DIALOG_DATA);
  public guid: string = this.data.guid;
  public instanceId: string = this.data.instanceId;

  public kickReason: WritableSignal<string> = signal("");

  public onKickClick(): void {
    InstanceService.getApiInstanceKickPlayer(this.guid, this.instanceId, this.kickReason()).finally(() => {
      this.dialogRef.close();
    });
  }

  onNoClick(): void {
    this.dialogRef.close();
  }
}
