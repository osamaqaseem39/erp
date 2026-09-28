import { Injectable } from '@nestjs/common';

@Injectable()
export class ProcurementService {
  getHello(): string {
    return 'Hello World!';
  }
}
