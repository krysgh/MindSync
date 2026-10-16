IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'MindSyncDb')
BEGIN
    CREATE DATABASE MindSyncDb;
END
GO

USE MindSyncDb;
GO

-- TABELA: Users

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id]           UNIQUEIDENTIFIER NOT NULL,
        [FirstName]    VARCHAR(50)      NOT NULL,
        [LastName]     VARCHAR(50)      NOT NULL,
        [Email]        VARCHAR(100)     NOT NULL,
        [PasswordHash] VARCHAR(255)     NOT NULL,
        [CreatedAt]    DATETIME         NOT NULL,
        
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email]
        ON [dbo].[Users] ([Email] ASC);
END
GO

-- TABELA: StressSessions

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StressSessions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StressSessions] (
        [Id]                 UNIQUEIDENTIFIER NOT NULL,
        [UserId]             UNIQUEIDENTIFIER NOT NULL,
        [CreatedAt]          DATETIME         NOT NULL,
        [EndedAt]            DATETIME         NULL,
        [StressLevelBefore]  INT              NOT NULL, 
        [StressLevelAfter]   INT              NULL,
        [TargetFrequency]    VARCHAR(50)      NOT NULL,

        CONSTRAINT [PK_StressSessions] PRIMARY KEY CLUSTERED ([Id] ASC),
        
        CONSTRAINT [FK_StressSessions_Users_UserId] FOREIGN KEY ([UserId]) 
            REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_StressSessions_UserId_CreatedAt]
        ON [dbo].[StressSessions] ([UserId] ASC, [CreatedAt] DESC);
END
GO

-- TABELA: FavoritedLists

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FavoritedLists]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FavoritedLists] (
        [Id]        UNIQUEIDENTIFIER NOT NULL,
        [UserId]    UNIQUEIDENTIFIER NOT NULL,
        [Name]      VARCHAR(100)     NOT NULL,
        [CreatedAt] DATETIME         NOT NULL,

        CONSTRAINT [PK_FavoritedLists] PRIMARY KEY CLUSTERED ([Id] ASC),

        CONSTRAINT [FK_FavoritedLists_Users_UserId] FOREIGN KEY ([UserId])
            REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_FavoritedLists_UserId_CreatedAt]
        ON [dbo].[FavoritedLists] ([UserId] ASC, [CreatedAt] DESC);
END
GO

-- TABELA: FavoritedSessions

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[FavoritedSessions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[FavoritedSessions] (
        [Id]              UNIQUEIDENTIFIER NOT NULL,
        [FavoritedListId]  UNIQUEIDENTIFIER NOT NULL,
        [StressSessionId] UNIQUEIDENTIFIER NOT NULL,
        [CustomName]      VARCHAR(100)     NOT NULL,
        [FavoritedAt]     DATETIME         NOT NULL,

        CONSTRAINT [PK_FavoritedSessions] PRIMARY KEY CLUSTERED ([Id] ASC),

        CONSTRAINT [FK_FavoritedSessions_FavoritedLists_FavoritedListId] FOREIGN KEY ([FavoritedListId])
            REFERENCES [dbo].[FavoritedLists] ([Id]) ON DELETE CASCADE,

        CONSTRAINT [FK_FavoritedSessions_StressSessions_StressSessionId] FOREIGN KEY ([StressSessionId])
            REFERENCES [dbo].[StressSessions] ([Id]) ON DELETE NO ACTION
    );

    CREATE NONCLUSTERED INDEX [IX_FavoritedSessions_FavoritedListId]
        ON [dbo].[FavoritedSessions] ([FavoritedListId] ASC);

    CREATE NONCLUSTERED INDEX [IX_FavoritedSessions_StressSessionId]
        ON [dbo].[FavoritedSessions] ([StressSessionId] ASC);
END
GO

-- TABELA: AudioTracks

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AudioTracks]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AudioTracks] (
        [Id]               UNIQUEIDENTIFIER NOT NULL,
        [Name]             VARCHAR(100)     NOT NULL,
        [Role]             VARCHAR(30)      NOT NULL, 
        [TargetFrequency]  VARCHAR(50)      NULL, 
        [MinStressLevel]   INT              NOT NULL,
        [MaxStressLevel]   INT              NOT NULL,
        [FileName]         VARCHAR(255)     NOT NULL,
        [DurationSeconds]  INT              NOT NULL,
        [CreatedAt]        DATETIME         NOT NULL,

        CONSTRAINT [PK_AudioTracks] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_AudioTracks_StressRange] CHECK ([MinStressLevel] <= [MaxStressLevel]),
        CONSTRAINT [CK_AudioTracks_StressBounds] CHECK ([MinStressLevel] BETWEEN 0 AND 8 AND [MaxStressLevel] BETWEEN 0 AND 8)
    );

    CREATE NONCLUSTERED INDEX [IX_AudioTracks_Role_TargetFrequency]
        ON [dbo].[AudioTracks] ([Role] ASC, [TargetFrequency] ASC);
END
GO

-- TABELA: SessionAudioMixes

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SessionAudioMixes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SessionAudioMixes] (
        [Id]               UNIQUEIDENTIFIER NOT NULL,
        [StressSessionId]  UNIQUEIDENTIFIER NOT NULL,
        [CreatedAt]        DATETIME         NOT NULL,

        CONSTRAINT [PK_SessionAudioMixes] PRIMARY KEY CLUSTERED ([Id] ASC),

        CONSTRAINT [FK_SessionAudioMixes_StressSessions_StressSessionId] FOREIGN KEY ([StressSessionId])
            REFERENCES [dbo].[StressSessions] ([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE NONCLUSTERED INDEX [IX_SessionAudioMixes_StressSessionId]
        ON [dbo].[SessionAudioMixes] ([StressSessionId] ASC);
END
GO

-- TABELA: SessionAudioMixLayers

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SessionAudioMixLayers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SessionAudioMixLayers] (
        [Id]                 UNIQUEIDENTIFIER NOT NULL,
        [SessionAudioMixId]  UNIQUEIDENTIFIER NOT NULL,
        [AudioTrackId]       UNIQUEIDENTIFIER NOT NULL,
        [Volume]             DECIMAL(4,3)     NOT NULL DEFAULT 1.0,
        [CreatedAt]          DATETIME         NOT NULL,

        CONSTRAINT [PK_SessionAudioMixLayers] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [CK_SessionAudioMixLayers_Volume] CHECK ([Volume] BETWEEN 0 AND 1),

        CONSTRAINT [FK_SessionAudioMixLayers_SessionAudioMixes_SessionAudioMixId] FOREIGN KEY ([SessionAudioMixId])
            REFERENCES [dbo].[SessionAudioMixes] ([Id]) ON DELETE CASCADE,

        CONSTRAINT [FK_SessionAudioMixLayers_AudioTracks_AudioTrackId] FOREIGN KEY ([AudioTrackId])
            REFERENCES [dbo].[AudioTracks] ([Id]) ON DELETE NO ACTION,

        CONSTRAINT [UQ_SessionAudioMixLayers_Mix_Track] UNIQUE ([SessionAudioMixId], [AudioTrackId])
    );

    CREATE NONCLUSTERED INDEX [IX_SessionAudioMixLayers_SessionAudioMixId]
        ON [dbo].[SessionAudioMixLayers] ([SessionAudioMixId] ASC);

    CREATE NONCLUSTERED INDEX [IX_SessionAudioMixLayers_AudioTrackId]
        ON [dbo].[SessionAudioMixLayers] ([AudioTrackId] ASC);
END
GO