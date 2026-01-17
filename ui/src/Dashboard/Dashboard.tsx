import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import Board from "../Components/Board/Board";
import "./Dashboard.css";
import CustomInput from "../Components/CustomInput/CustomInput";
import { useAppDispatch, useAppSelector } from "../store/hooks";
import {
  fetchBoards,
  createBoard as createBoardAction,
  deleteBoard as deleteBoardAction,
  updateBoardTitle,
  addCard,
  removeCard,
  moveCard,
  moveBoard,
  clearError,
} from "../store/slices/boardsSlice";
import { logout as logoutAction } from "../store/slices/authSlice";
import { usePermissions } from "../utils/permissions";

const ICON_LOGO_URL = "https://www.icon.com.mt/wp-content/themes/icon-2024update/img/logo.svg?v25";

function Dashboard() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);
  const { boards, isLoading, error } = useAppSelector((state) => state.boards);
  const permissions = usePermissions(user?.role);

  const [targetCard, setTargetCard] = useState({
    boardId: "",
    cardId: "",
  });

  const [targetBoard, setTargetBoard] = useState<string>("");
  const [isDraggingBoard, setIsDraggingBoard] = useState(false);

  useEffect(() => {
    if (user?.id) {
      dispatch(fetchBoards());
    }
  }, [user, dispatch]);
  const addboardHandler = async (name: string) => {
    if (!user?.id) return;
    dispatch(createBoardAction({ title: name, userId: user.id }));
  };

  const removeBoardHandler = async (boardId: string) => {
    dispatch(deleteBoardAction(boardId));
  };

  const updateBoardHandler = async (boardId: string, title: string) => {
    dispatch(updateBoardTitle({ boardId, title }));
  };

  const addCardHandler = (boardId: string, title: string) => {
    if (!user?.id) return;
    dispatch(addCard({ boardId, title, userId: user.id }));
  };

  const removeCardHandler = (boardId: string, cardId: string) => {
    dispatch(removeCard({ boardId, cardId }));
  };

  const onDragEnd = (boardId: string, cardId: string) => {
    if (!targetCard.boardId || !targetCard.cardId) return;

    dispatch(
      moveCard({
        sourceBoardId: boardId,
        targetBoardId: targetCard.boardId,
        sourceCardId: cardId,
        targetCardId: targetCard.cardId,
      })
    );

    setTargetCard({
      boardId: "",
      cardId: "",
    });
  };

  const onDragEnter = (boardId: string, cardId: string) => {
    if (targetCard.cardId === cardId) return;
    setTargetCard({
      boardId: boardId,
      cardId: cardId,
    });
  };

  const handleLogout = () => {
    dispatch(logoutAction());
    navigate('/login');
  };

  const handleClearError = () => {
    dispatch(clearError());
  };

  const onBoardDragStart = (e: React.DragEvent, boardId: string) => {
    setIsDraggingBoard(true);
    e.dataTransfer.effectAllowed = 'move';
  };

  const onBoardDragEnter = (e: React.DragEvent, targetBoardId: string) => {
    e.preventDefault();
    if (!isDraggingBoard) return;
    if (targetBoard === targetBoardId) return;
    setTargetBoard(targetBoardId);
  };

  const onBoardDragEnd = (sourceBoardId: string) => {
    if (!isDraggingBoard) return;

    if (!targetBoard || targetBoard === sourceBoardId) {
      setTargetBoard("");
      setIsDraggingBoard(false);
      return;
    }

    dispatch(
      moveBoard({
        sourceBoardId,
        targetBoardId: targetBoard,
      })
    );

    setTargetBoard("");
    setIsDraggingBoard(false);
  };

  if (isLoading) {
    return (
      <div className="app">
        <div className="app-nav">
          <h1>Task Management System</h1>
          <div className="app-nav-right">
            <span className="user-email">{user?.email}</span>
            <button onClick={handleLogout} className="logout-button">
              Logout
            </button>
          </div>
        </div>
        <div className="app-boards-container">
          <div style={{ textAlign: 'center', padding: '50px' }}>
            Loading boards...
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="app">
      <div className="app-nav">
        <div className="app-nav-left">
          <div className="app-logo">
            <div className="app-logo-icon">
              <img src={ICON_LOGO_URL} alt="ICON Logo" />
            </div>
            <span>Task Management</span>
          </div>
        </div>
        <div className="app-nav-right">
          <span className="user-email">
            {user?.email} {user?.role && <span className="user-role">({user.role})</span>}
          </span>
          {permissions.isAdmin && (
            <button
              onClick={() => navigate('/admin')}
              className="logout-button"
              style={{ background: 'rgba(255, 255, 255, 0.1)', marginRight: '10px' }}
            >
              Admin Panel
            </button>
          )}
          <button onClick={handleLogout} className="logout-button">
            Logout
          </button>
        </div>
      </div>
      {error && (
        <div style={{
          backgroundColor: '#f44336',
          color: 'white',
          padding: '10px',
          textAlign: 'center'
        }}>
          {error}
          <button
            onClick={handleClearError}
            style={{ marginLeft: '10px', cursor: 'pointer' }}
          >
            ✕
          </button>
        </div>
      )}
      <div className="app-boards-container">
        <div className="app-boards">
          {boards.map((item) => (
            <Board
              key={item.id}
              board={item}
              addCard={addCardHandler}
              removeBoard={() => removeBoardHandler(item.id)}
              updateBoard={(boardId, title) => updateBoardHandler(boardId, title)}
              removeCard={removeCardHandler}
              onDragEnd={onDragEnd}
              onDragEnter={onDragEnter}
              onBoardDragStart={(e) => onBoardDragStart(e, item.id)}
              onBoardDragEnter={(e) => onBoardDragEnter(e, item.id)}
              onBoardDragEnd={() => onBoardDragEnd(item.id)}
              isDraggingBoard={isDraggingBoard}
              isTargetBoard={targetBoard === item.id}
            />
          ))}
          {permissions.canCreateBoard && (
            <div key="add-board" className="app-boards-last">
              <CustomInput
                displayClass="app-boards-add-board"
                editClass="app-boards-add-board-edit"
                placeholder="Enter Board Name"
                text="Add Board"
                buttonText="Add Board"
                onSubmit={addboardHandler}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
