-- SEED: AudioTracks
-- Reexecutável: só insere faixas cujo FileName ainda não existe.
-- Pra adicionar novas faixas no futuro, é só ACRESCENTAR linhas no VALUES abaixo e rodar o script de novo — as já existentes são ignoradas.

INSERT INTO [dbo].[AudioTracks] (Id, Name, Role, TargetFrequency, MinStressLevel, MaxStressLevel, FileName, DurationSeconds, CreatedAt)
SELECT NEWID(), v.Name, v.Role, v.TargetFrequency, v.MinStressLevel, v.MaxStressLevel, v.FileName, v.DurationSeconds, GETUTCDATE()
FROM (VALUES
    -- Binaural
    ('Binaural Theta 5Hz', 'Binaural', 'Theta (6 Hz) - Meditação', 0, 8, 'binaural/theta-5hz.mp3', 600),
    ('Binaural Delta 2Hz', 'Binaural', 'Delta (2 Hz) - Sono Profundo', 0, 8, 'binaural/delta-2hz.mp3', 600),
    ('Binaural Alpha 10Hz', 'Binaural', 'Alpha (10 Hz) - Relaxamento', 0, 8, 'binaural/alpha-10hz.mp3', 600),
    ('Binaural Beta 20Hz', 'Binaural', 'Beta (20 Hz) - Foco e Atenção', 0, 8, 'binaural/beta-20hz.mp3', 600),

    -- Texturas: estresse baixo (0-2)
    ('Sinos de Vento', 'Texture', NULL, 0, 2, 'texture/wind-chimes-bells.mp3', 600),
    ('Tigelas Tibetanas', 'Texture', NULL, 0, 2, 'texture/meditation-bowls.mp3', 600),
    ('Ruído Rosa', 'Texture', NULL, 0, 2, 'texture/pink-noise.mp3', 600),

    -- Texturas: estresse moderado (3-5)
    ('Cachoeira', 'Texture', NULL, 3, 5, 'texture/waterfall.mp3', 600),
    ('Pássaros', 'Texture', NULL, 3, 5, 'texture/birds.mp3', 600),
    ('Ondas do Mar', 'Texture', NULL, 3, 5, 'texture/sea-wave.mp3', 600),
    ('Rio', 'Texture', NULL, 3, 5, 'texture/river.mp3', 600),
    ('Vento', 'Texture', NULL, 3, 5, 'texture/wind-artificial.mp3', 600),

    -- Texturas: estresse alto (6-8)
    ('Chuva Leve', 'Texture', NULL, 6, 8, 'texture/light-rain.mp3', 600),
    ('Temporal ao Longe', 'Texture', NULL, 6, 8, 'texture/thunderstorm-with-distant-birds.mp3', 600),
    ('Ruído Marrom', 'Texture', NULL, 6, 8, 'texture/smoothed-brown-noise.mp3', 600),
    ('Ruído Branco Suave', 'Texture', NULL, 6, 8, 'texture/deep-white-noise.mp3',  600)
) AS v(Name, Role, TargetFrequency, MinStressLevel, MaxStressLevel, FileName, DurationSeconds)
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[AudioTracks] existing WHERE existing.FileName = v.FileName
);
GO