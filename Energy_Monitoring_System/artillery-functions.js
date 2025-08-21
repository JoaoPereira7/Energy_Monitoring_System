const activeUsers = new Map();
let lastMemoryCheck = Date.now();
let lastHealthCheck = Date.now();

module.exports = {
  // Função para gerar timestamp válido (no passado)
  generateValidTimestamp: function(context, events, done) {
    const now = new Date();
    const pastTime = new Date(now.getTime() - Math.random() * 60 * 60 * 1000); // Até 1 hora atrás
    context.vars.validTimestamp = pastTime.toISOString();
    return done();
  },

  // Função para gerar dados de leitura realistas
  generateLeituraData: function(context, events, done) {
    // Gerar dados dentro de faixas realistas para medidores de energia
    const tensaoBase = 220; // Tensão nominal brasileira
    const tensao = (tensaoBase + (Math.random() - 0.5) * 20).toFixed(2); // ±10V
    
    const corrente = (Math.random() * 15 + 5).toFixed(2); // 5-20A
    
    const potenciaAtiva = (parseFloat(tensao) * parseFloat(corrente) * (0.8 + Math.random() * 0.2)).toFixed(2);
    
    const potenciaReativa = (parseFloat(potenciaAtiva) * (Math.random() * 0.5)).toFixed(2);
    
    const energiaAtivaDireta = (Math.random() * 2000 + 1000).toFixed(2);
    
    const energiaAtivaReversa = (Math.random() * 100 + 20).toFixed(2);
    
    const fatorPotencia = (0.8 + Math.random() * 0.19).toFixed(3); // 0.8-0.99
    
    const frequencia = (59.5 + Math.random() * 1).toFixed(1); // 59.5-60.5 Hz

    context.vars.leituraData = {
      tensao: parseFloat(tensao),
      corrente: parseFloat(corrente),
      potenciaAtiva: parseFloat(potenciaAtiva),
      potenciaReativa: parseFloat(potenciaReativa),
      energiaAtivaDireta: parseFloat(energiaAtivaDireta),
      energiaAtivaReversa: parseFloat(energiaAtivaReversa),
      fatorPotencia: parseFloat(fatorPotencia),
      frequencia: parseFloat(frequencia)
    };

    // Validar dados gerados
    this.validateDataConsistency(context, events, done);

    return done();
  },

  // Função para log de performance crítica
  logPerformanceMetrics: function(context, events, done) {
    const responseTime = context.vars.$responseTime;
    const endpoint = context.vars.$requestUrl;
    
    if (responseTime > 2000) {
      console.log(`?? ALERTA: Resposta lenta detectada - ${responseTime}ms no endpoint ${endpoint}`);
      
      // Ajuste dinâmico de taxa de requisições
      if (context.vars.arrivalRate) {
        context.vars.arrivalRate = Math.max(1, Math.floor(context.vars.arrivalRate * 0.8));
        console.log(`?? Ajustando taxa de chegada para ${context.vars.arrivalRate} devido à alta latência`);
      }
    }
    
    // Monitorar padrões de erro
    if (context.vars.$statusCode >= 400) {
      console.log(`?? Erro detectado: Status ${context.vars.$statusCode} em ${endpoint}`);
      console.log(`Tempo de resposta: ${responseTime}ms`);
    }
    
    return done();
  },

  // Função para gerar período de consulta válido
  generateQueryPeriod: function(context, events, done) {
    const now = new Date();
    const hoursBack = Math.floor(Math.random() * 24) + 1; // 1-24 horas atrás
    
    const dataFim = new Date(now.getTime() - Math.random() * 3600000); // Até 1h atrás
    const dataInicio = new Date(dataFim.getTime() - hoursBack * 3600000);
    
    context.vars.dataInicio = dataInicio.toISOString();
    context.vars.dataFim = dataFim.toISOString();
    
    return done();
  },

  // Função para monitorar métricas do banco de dados
  monitorDatabaseMetrics: function(context, events, done) {
    const responseTime = context.vars.$responseTime;
    const endpoint = context.vars.$requestUrl;

    if (responseTime > 1000) {
      console.log(`?? Alerta de Performance do Banco de Dados:`);
      console.log(`Endpoint: ${endpoint}`);
      console.log(`Tempo de Resposta: ${responseTime}ms`);
      
      if (endpoint.includes('leituras')) {
        console.log(`?? Possível gargalo em consultas de leituras`);
      }
    }
    return done();
  },

  // Função para validar consistência dos dados
  validateDataConsistency: function(context, events, done) {
    if (context.vars.leituraData) {
      const data = context.vars.leituraData;
      const isValid = data.tensao >= 180 && data.tensao <= 260 &&
                     data.corrente >= 0 && data.corrente <= 100 &&
                     data.frequencia >= 55 && data.frequencia <= 65 &&
                     data.fatorPotencia >= 0 && data.fatorPotencia <= 1;
      
      if (!isValid) {
        console.log('?? Dados inválidos detectados:', data);
      }
    }
    return done();
  },

  // Função para monitorar uso de memória
  trackMemoryUsage: function(context, events, done) {
    const now = Date.now();
    if (now - lastMemoryCheck > 60000) { // Checar a cada minuto
      const used = process.memoryUsage();
      lastMemoryCheck = now;
      
      const heapUsedMB = Math.round(used.heapUsed / 1024 / 1024);
      const heapTotalMB = Math.round(used.heapTotal / 1024 / 1024);
      
      if (heapUsedMB > 500) { // Alerta se uso passar de 500MB
        console.log('?? Alto Uso de Memória:');
        console.log(`Heap Usado: ${heapUsedMB}MB de ${heapTotalMB}MB total`);
      }
    }
    return done();
  },

  // Função para monitorar usuários concorrentes
  trackConcurrentUsers: function(context, events, done) {
    const currentTime = Date.now();
    const userId = context.vars.userId || `user_${Math.random()}`;
    activeUsers.set(userId, currentTime);
    
    // Limpar entradas antigas
    activeUsers.forEach((timestamp, id) => {
      if (currentTime - timestamp > 60000) { // Remover após 1 minuto
        activeUsers.delete(id);
      }
    });
    
    if (activeUsers.size > 1000) {
      console.log(`?? Alto número de usuários concorrentes: ${activeUsers.size}`);
    }
    
    return done();
  },

  // Função para monitorar saúde da API
  monitorApiHealth: function(context, events, done) {
    const now = Date.now();
    const statusCode = context.vars.$statusCode;
    
    if (now - lastHealthCheck > 30000) { // Verificar a cada 30 segundos
      lastHealthCheck = now;
      
      if (statusCode >= 500) {
        console.log('?? Falha na Saúde da API:');
        console.log(`Status: ${statusCode}`);
        console.log(`Endpoint: ${context.vars.$requestUrl}`);
        console.log(`Tempo de Resposta: ${context.vars.$responseTime}ms`);
      }
    }
    
    return done();
  }
};