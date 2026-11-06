import { HttpErrorResponse } from '@angular/common/http';

export function readApiError(response: HttpErrorResponse) {
  const errors = response.error?.errors as Record<string, string[]> | undefined;
  const detail = response.error?.detail as string | undefined;

  return {
    message: (errors && Object.values(errors).flat().join(' ')) ||
      detail ||
      response.error?.title ||
      (response.status === 0 ? 'Não foi possível conectar à API.' : 'Não foi possível concluir a operação.'),
    fields: errors ? Object.keys(errors).map(field => field.toLowerCase()) : [],
  };
}
