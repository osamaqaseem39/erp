import { Injectable } from '@nestjs/common';

@Injectable()
export class ManufacturingService {
  getHello(): string {
    return 'Hello World!';
  }
}
