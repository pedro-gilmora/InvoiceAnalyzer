-- Create the new database
CREATE DATABASE "rag-db";

-- Connect to the newly created database
\c "rag-db"

-- Enable the pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;