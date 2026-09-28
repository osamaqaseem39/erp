import { NestFactory } from '@nestjs/core';
import { ProcurementModule } from './procurement.module';

async function bootstrap() {
  const app = await NestFactory.create(ProcurementModule);
  await app.listen(process.env.port ?? 3000);
}
bootstrap();
