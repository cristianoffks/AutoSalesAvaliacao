import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  ViewChild,
} from '@angular/core';

@Component({
  changeDetection: ChangeDetectionStrategy.Eager,
  selector: 'app-error-dialog',
  templateUrl: './error-dialog.component.html',
})
export class ErrorDialogComponent implements OnChanges, AfterViewInit {
  @Input() title = 'Atenção';
  @Input() message = '';
  @Output() closed = new EventEmitter<void>();
  @ViewChild('dialog') private dialog?: ElementRef<HTMLDialogElement>;

  ngOnChanges(): void {
    this.openDialog();
  }

  ngAfterViewInit(): void {
    this.openDialog();
  }

  private openDialog(): void {
    const dialog = this.dialog?.nativeElement;
    if (this.message && dialog && !dialog.open) {
      dialog.showModal();
    }
  }
}
