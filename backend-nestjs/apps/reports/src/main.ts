import { NestFactory } from '@nestjs/core';
import { ReportsModule } from './reports.module';

async function bootstrap() {
  const app = await NestFactory.create(ReportsModule);
  await app.listen(process.env.port ?? 3000);
}
bootstrap();
