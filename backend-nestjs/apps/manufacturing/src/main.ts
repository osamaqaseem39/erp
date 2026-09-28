import { NestFactory } from '@nestjs/core';
import { ManufacturingModule } from './manufacturing.module';

async function bootstrap() {
  const app = await NestFactory.create(ManufacturingModule);
  await app.listen(process.env.port ?? 3000);
}
bootstrap();
