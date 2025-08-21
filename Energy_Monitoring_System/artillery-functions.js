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

    return done();
  },

  // Função para log de performance crítica
  logPerformanceMetrics: function(context, events, done) {
    const responseTime = context.vars.$responseTime;
    
    if (responseTime > 2000) {
      console.log(`??  ALERTA: Resposta lenta detectada - ${responseTime}ms`);
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
  }
};