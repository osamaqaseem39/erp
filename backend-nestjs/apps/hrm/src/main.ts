import { NestFactory } from '@nestjs/core';
import { HrmModule } from './hrm.module';

async function bootstrap() {
  const app = await NestFactory.create(HrmModule);
  await app.listen(process.env.port ?? 3000);
}
bootstrap();
