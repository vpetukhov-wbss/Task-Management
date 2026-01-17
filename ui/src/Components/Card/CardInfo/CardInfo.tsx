import React, { useEffect, useState } from "react";
import { List, Type } from "react-feather";
import Modal from "../../Modal/Modal";
import CustomInput from "../../CustomInput/CustomInput";

import "./CardInfo.css";
import { ICard, TaskPriority, TaskStatus } from "../../../Interfaces/Kanban";
import { useAppDispatch, useAppSelector } from "../../../store/hooks";
import { patchCard } from "../../../store/slices/boardsSlice";
import { usePermissions } from "../../../utils/permissions";

interface CardInfoProps {
  onClose: () => void;
  card: ICard;
  boardId: string;
}
function CardInfo(props: CardInfoProps) {
  const { onClose, card, boardId } = props;
  const dispatch = useAppDispatch();
  const { user } = useAppSelector((state) => state.auth);
  const permissions = usePermissions(user?.role);
  const [cardValues, setCardValues] = useState<ICard>({
    ...card,
  });

  // Track which fields have changed
  const [changedFields, setChangedFields] = useState<{
    title?: boolean;
    description?: boolean;
    priority?: boolean;
    status?: boolean;
  }>({});

  const updateTitle = (value: string) => {
    setCardValues({ ...cardValues, title: value });
    setChangedFields({ ...changedFields, title: true });
  };

  const updateDesc = (value: string) => {
    setCardValues({ ...cardValues, description: value });
    setChangedFields({ ...changedFields, description: true });
  };

  const updatePriority = (priority: TaskPriority) => {
    setCardValues({
      ...cardValues,
      priority,
      priorityName: TaskPriority[priority]
    });
    setChangedFields({ ...changedFields, priority: true });
  };

  const updateStatus = (status: TaskStatus) => {
    setCardValues({
      ...cardValues,
      status,
      statusName: TaskStatus[status]
    });
    setChangedFields({ ...changedFields, status: true });
  };

  useEffect(() => {
    if (Object.keys(changedFields).length === 0) {
      return;
    }

    const saveChanges = () => {
      const updates: { title?: string; description?: string; priority?: TaskPriority; status?: TaskStatus } = {};

      if (changedFields.title) {
        updates.title = cardValues.title;
      }
      if (changedFields.description) {
        updates.description = cardValues.description || '';
      }
      if (changedFields.priority && cardValues.priority !== undefined) {
        updates.priority = cardValues.priority;
      }
      if (changedFields.status && cardValues.status !== undefined) {
        updates.status = cardValues.status;
      }

      if (Object.keys(updates).length > 0) {
        dispatch(
          patchCard({
            boardId,
            cardId: cardValues.id,
            taskId: cardValues.id,
            updates,
          })
        );
        setChangedFields({});
      }
    };

    const timeoutId = setTimeout(saveChanges, 1000);
    return () => {
      clearTimeout(timeoutId);
      if (Object.keys(changedFields).length > 0) {
        saveChanges();
      }
    };
  }, [cardValues, changedFields, boardId, dispatch]);

  return (
    <Modal onClose={onClose}>
      <div className="cardinfo">
        {permissions.isReadOnly && (
          <div style={{
            backgroundColor: '#fff3cd',
            color: '#856404',
            padding: '10px',
            borderRadius: '5px',
            marginBottom: '15px',
            textAlign: 'center'
          }}>
            ℹ️ You have read-only access. Contact an Admin or Manager to make changes.
          </div>
        )}
        <div className="cardinfo-box">
          <div className="cardinfo-box-title">
            <Type />
            <p>Title</p>
          </div>
          {permissions.canManageTask ? (
            <CustomInput
              defaultValue={cardValues.title}
              text={cardValues.title}
              placeholder="Enter Title"
              onSubmit={updateTitle}
            />
          ) : (
            <div style={{ padding: '10px', backgroundColor: '#f8f9fa', borderRadius: '5px' }}>
              {cardValues.title}
            </div>
          )}
        </div>

        <div className="cardinfo-box">
          <div className="cardinfo-box-title">
            <List />
            <p>Description</p>
          </div>
          {permissions.canManageTask ? (
            <CustomInput
              defaultValue={cardValues.description || ''}
              text={cardValues.description || "Add a Description"}
              placeholder="Enter description"
              onSubmit={updateDesc}
            />
          ) : (
            <div style={{ padding: '10px', backgroundColor: '#f8f9fa', borderRadius: '5px' }}>
              {cardValues.description || 'No description'}
            </div>
          )}
        </div>

        <div className="cardinfo-box">
          <div className="cardinfo-box-title">
            <Type />
            <p>Priority</p>
          </div>
          <div className="cardinfo-priority-buttons">
            <button
              className={`priority-btn ${cardValues.priority === TaskPriority.Low ? 'active low' : ''}`}
              onClick={() => updatePriority(TaskPriority.Low)}
              disabled={!permissions.canManageTask}
            >
              Low
            </button>
            <button
              className={`priority-btn ${cardValues.priority === TaskPriority.Medium ? 'active medium' : ''}`}
              onClick={() => updatePriority(TaskPriority.Medium)}
              disabled={!permissions.canManageTask}
            >
              Medium
            </button>
            <button
              className={`priority-btn ${cardValues.priority === TaskPriority.High ? 'active high' : ''}`}
              onClick={() => updatePriority(TaskPriority.High)}
              disabled={!permissions.canManageTask}
            >
              High
            </button>
          </div>
        </div>

        <div className="cardinfo-box">
          <div className="cardinfo-box-title">
            <List />
            <p>Status</p>
          </div>
          <div className="cardinfo-status-buttons">
            <button
              className={`status-btn ${cardValues.status === TaskStatus.Incomplete ? 'active incomplete' : ''}`}
              onClick={() => updateStatus(TaskStatus.Incomplete)}
              disabled={!permissions.canManageTask}
            >
              Incomplete
            </button>
            <button
              className={`status-btn ${cardValues.status === TaskStatus.InProgress ? 'active inprogress' : ''}`}
              onClick={() => updateStatus(TaskStatus.InProgress)}
              disabled={!permissions.canManageTask}
            >
              In Progress
            </button>
            <button
              className={`status-btn ${cardValues.status === TaskStatus.OnHold ? 'active onhold' : ''}`}
              onClick={() => updateStatus(TaskStatus.OnHold)}
              disabled={!permissions.canManageTask}
            >
              On Hold
            </button>
            <button
              className={`status-btn ${cardValues.status === TaskStatus.Complete ? 'active complete' : ''}`}
              onClick={() => updateStatus(TaskStatus.Complete)}
              disabled={!permissions.canManageTask}
            >
              Complete
            </button>
          </div>
        </div>

        <div className="cardinfo-box">
          <div className="cardinfo-box-title">
            <List />
            <p>Metadata</p>
          </div>
          <div className="cardinfo-metadata">
            <p><strong>Created:</strong> {new Date(cardValues.createdAtUtc).toLocaleString()}</p>
            {cardValues.updatedAtUtc && (
              <p><strong>Updated:</strong> {new Date(cardValues.updatedAtUtc).toLocaleString()}</p>
            )}
          </div>
        </div>
      </div>
    </Modal>
  );
}

export default CardInfo;
