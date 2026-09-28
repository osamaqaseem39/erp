import { Injectable } from '@nestjs/common';

@Injectable()
export class ReportsService {
  getHello(): string {
    return 'Hello World!';
  }
}
