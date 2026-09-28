import { Injectable } from '@nestjs/common';

@Injectable()
export class HrmService {
  getHello(): string {
    return 'Hello World!';
  }
}
